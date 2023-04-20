import { useIsAuthenticated } from '@azure/msal-react'
import { SignInButton } from 'components/SignInButton'
import { User, UserService } from 'domain/services/UserService'
import { useService } from 'hooks/useService'
import { useEffect, useState } from 'react'
import styles from './ProfilePage.module.scss'

export const ProfilePage = () => {
    const userService = useService(UserService)
    const isAuthenticated = useIsAuthenticated()

    const [user, setUser] = useState<User>()

    useEffect(() => {
        if (isAuthenticated) {
            userService.getUser().then(user => {
                setUser(user)
            })
            // const account = userService.getUserAccount()
            // console.log('account', account)
        }
    }, [userService, isAuthenticated])

    return (
        <div className={styles.root}>
            <h1>Profile</h1>
            <p>
                This is your profile page. I demonstrates that you are authenticated with Azure
                Active Directory.
            </p>
            {isAuthenticated && <pre>{JSON.stringify(user, null, 2)}</pre>}
            {!isAuthenticated && (
                <span>
                    Sign in to view your pofile. <SignInButton />
                </span>
            )}
        </div>
    )
}
