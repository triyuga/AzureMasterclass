import { AccountInfo, IPublicClientApplication, SilentRequest } from '@azure/msal-browser'
import { loginRequest } from 'domain/authConfig'

export const getSessionId = () => localStorage.getItem('msal.session.state') || undefined

export abstract class AuthService {
    protected instance: IPublicClientApplication

    constructor(instance: IPublicClientApplication) {
        this.instance = instance
    }

    protected getAccessToken = async (): Promise<string> => {
        const accounts = this.instance.getAllAccounts()
        const request = {
            ...loginRequest,
            account: accounts.length > 0 ? accounts[0] : null,
            sid: getSessionId(),
        } as SilentRequest
        let token: string = ''
        try {
            const response = await this.instance.acquireTokenSilent(request)
            token = response.accessToken
        } catch (e) {
            this.instance.acquireTokenRedirect(loginRequest)
        }
        return token
    }

    protected getAuthenticatedUser(): AccountInfo | undefined {
        const accounts = this.instance.getAllAccounts()
        return accounts.find(x => x.username)
    }

    protected authenticate(): Promise<void> {
        return this.instance.loginRedirect(loginRequest).catch(e => {
            console.error('authentication error', e)
        })
    }

    protected logout() {
        return this.instance.logoutRedirect().catch(e => {
            console.error(e)
        })
    }
}
