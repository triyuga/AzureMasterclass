import { IPublicClientApplication } from '@azure/msal-browser'
import { HttpService } from './HttpService'

export interface EnabledFeatures {
    auth: boolean
    sql: boolean
    blobStorage: boolean
}

export class EnabledFeaturesService extends HttpService {
    constructor(instance: IPublicClientApplication) {
        const withAuthentication = false
        super(instance, withAuthentication)
    }

    public getEnabledFeatures = () => {
        return this.get<EnabledFeatures>('/api/enabledFeatures')
    }
}
