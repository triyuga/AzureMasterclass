import { useIsAuthenticated } from '@azure/msal-react'
import styles from './App.module.scss'
import { BrowserRouter, Link, Navigate, Route, Routes } from 'react-router-dom'
import { BooksPage } from 'views/BooksPage/BooksPage'
import { WeatherPage } from 'views/WeatherPage/WeatherPage'
import { SignOutButton } from 'components/SignOutButton'
import { SignInButton } from 'components/SignInButton'
import { HomePage } from 'views/HomePage/HomePage'
import { ProfilePage } from 'views/ProfilePage/ProfilePage'
import { useContext, useEffect } from 'react'
import { EnabledFeaturesContext } from 'contexts/EnabledFeaturesContext'

export function App() {
    const isAuthenticated = useIsAuthenticated()
    const { enabledFeatures } = useContext(EnabledFeaturesContext)

    useEffect(() => {
        const theme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
        document.documentElement.setAttribute('data-bs-theme', theme)
    }, [])

    return (
        <BrowserRouter>
            <div className={styles.root}>
                <div className={styles.header}>
                    <Link to='/' className={styles.brand}>
                        Azure Masterclass
                    </Link>
                    <nav className={styles.navigation}>
                        <ul>
                            <li>
                                <Link to='/home'>Home</Link>
                            </li>
                            <li>
                                <Link to='/weather'>Weather</Link>
                            </li>
                            {enabledFeatures.sql && (
                                <li>
                                    <Link to='/books'>Books</Link>
                                </li>
                            )}
                        </ul>
                    </nav>

                    <div className={styles.userNav}>
                        {enabledFeatures.auth && (
                            <>
                                <Link to='/profile'>Profile</Link>
                                {isAuthenticated ? (
                                    <>
                                        <SignOutButton />
                                    </>
                                ) : (
                                    <SignInButton />
                                )}
                            </>
                        )}
                    </div>
                </div>
                <Routes>
                    <Route path='/home' element={<HomePage />} />
                    <Route path='/weather' element={<WeatherPage />} />
                    {enabledFeatures.sql && <Route path='/books' element={<BooksPage />} />}
                    {enabledFeatures.auth && <Route path='/profile' element={<ProfilePage />} />}
                    <Route path='/' element={<Navigate replace to='/home' />} />
                </Routes>
            </div>
        </BrowserRouter>
    )
}
