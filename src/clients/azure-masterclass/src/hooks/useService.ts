import { IPublicClientApplication } from '@azure/msal-browser'
import { useMsal } from '@azure/msal-react'
import { useMemo } from 'react'
import { HttpService } from 'domain/services/HttpService'

export const useService = <T extends HttpService>(service: {
    new (instance: IPublicClientApplication): T
}): T => {
    const { instance } = useMsal()
    const instantiatedService = useMemo(() => new service(instance), [service, instance])
    return instantiatedService
}
