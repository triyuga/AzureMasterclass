import { IPublicClientApplication } from '@azure/msal-browser'
import axios, { AxiosRequestConfig, AxiosResponse } from 'axios'
import { config } from 'domain/config'
import { AuthService, getSessionId } from './AuthService'

const http = axios.create({
    baseURL: config.apiUrl,
    withCredentials: false,
})

const AuditCorrelationIdHeader = 'audit-corrid'

let auditCorrId: string | null = null

http.interceptors.request.use(
    async request => {
        request.headers!['Site-Planning-Session-Id'] = getSessionId() || ''
        if (auditCorrId) {
            request.headers![AuditCorrelationIdHeader] = auditCorrId
        }
        return request
    },
    error => Promise.reject(error)
)

http.interceptors.response.use(
    async response => {
        if (response.headers![AuditCorrelationIdHeader]) {
            auditCorrId = response.headers![AuditCorrelationIdHeader]
        }
        return response
    },
    error => {
        if (error.request) {
            const response = error.response
            let errorMsg = 'An unknown error has occurred. Please try again.'
            if (response && response.data && response.data.message) {
                errorMsg = response.data.message
            } else if (response && response.data && typeof response.data === 'string') {
                errorMsg = response.data
            } else if (response && response.status === 403) {
                errorMsg = 'You are not authorised to view this page.'
            } else if (response && response.status === 404) {
                errorMsg = 'The information you are looking for was not found.'
            } else if (response && response.status === 503) {
                errorMsg = 'System is undergoing maintenance, please try again in a moment.'
            }

            console.error(errorMsg)
        }

        return axios.isCancel(error) ? Promise.resolve() : Promise.reject(error)
    }
)

export abstract class HttpService extends AuthService {
    private withAuthentication: boolean

    constructor(instance: IPublicClientApplication, withAuthentication: boolean = true) {
        super(instance)
        this.withAuthentication = withAuthentication
    }

    protected async get<T>(url: string, config?: Partial<AxiosRequestConfig>): Promise<T> {
        const httpConfig = await this.makeHttpConfig(config)
        return http.get(url, httpConfig).then(res => res.data)
    }

    protected async post<T, R>(
        url: string,
        data?: T,
        config?: Partial<AxiosRequestConfig>
    ): Promise<R> {
        const httpConfig = await this.makeHttpConfig(config)
        return http.post<T, AxiosResponse<R>>(url, data, httpConfig).then(res => res.data)
    }

    protected async put<T, R>(
        url: string,
        data?: T,
        config?: Partial<AxiosRequestConfig>
    ): Promise<R> {
        const httpConfig = await this.makeHttpConfig(config)
        return http.put<T, AxiosResponse<R>>(url, data, httpConfig).then(res => res.data)
    }

    protected async delete<T, R>(url: string, config?: Partial<AxiosRequestConfig>): Promise<R> {
        const httpConfig = await this.makeHttpConfig(config)
        return http.delete<T, AxiosResponse<R>>(url, httpConfig).then(res => res.data)
    }

    private async makeHttpConfig(config?: Partial<AxiosRequestConfig>) {
        if (!this.withAuthentication) {
            return { ...(config || {}) }
        }

        const token = await this.getAccessToken()
        return {
            ...(config || {}),
            headers: { ...(config?.headers || {}), authorization: `Bearer ${token}` },
        }
    }
}
