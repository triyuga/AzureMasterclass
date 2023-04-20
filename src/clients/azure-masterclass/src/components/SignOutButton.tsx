import { useMsal } from '@azure/msal-react'
import Button from 'react-bootstrap/Button'

export const SignOutButton = () => {
    const { instance } = useMsal()

    const handleLogout = () => {
        instance.logoutRedirect({
            postLogoutRedirectUri: '/',
        })
    }

    return (
        <Button variant='secondary' className='ml-auto' onClick={() => handleLogout()}>
            Sign out
        </Button>
    )
}
