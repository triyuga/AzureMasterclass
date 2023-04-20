import { useEffect, useState } from 'react'
import { WeatherForecast, WeatherService } from 'domain/services/WeatherService'
import { useService } from 'hooks/useService'
import styles from './WeatherPage.module.scss'

export const WeatherPage = () => {
    const weatherService = useService(WeatherService)

    const [weather, setWeather] = useState<WeatherForecast[]>([])

    useEffect(() => {
        weatherService.getWeather().then(weather => {
            setWeather(weather)
        })
    }, [weatherService])

    return (
        <div className={styles.root}>
            <h1>Weather</h1>
            <p>
                This is the weather page. It calls a public endpoint to get some example data, as
                per the default weather controller created with{' '}
                <code>dotnet new --template webapi</code>.
            </p>
            <pre>{JSON.stringify(weather, null, 2)}</pre>
        </div>
    )
}
