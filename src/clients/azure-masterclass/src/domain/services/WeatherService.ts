import { IPublicClientApplication } from '@azure/msal-browser'
import { HttpService } from './HttpService'

export interface WeatherForecast {
    date?: Date
    temperatureC?: number
    readonly temperatureF?: number
    summary?: string
}

export class WeatherService extends HttpService {
    constructor(instance: IPublicClientApplication) {
        const withAuthentication = false
        super(instance, withAuthentication)
    }

    public getWeather = () => {
        return this.get<WeatherForecast[]>('/api/weatherForecast').then(res => {
            console.log('res', res)
            return res
        })
    }
}
