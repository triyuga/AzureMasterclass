import { useMsal } from '@azure/msal-react'
import { loginRequest } from 'domain/authConfig'
import Button from 'react-bootstrap/Button'

export const SignInButton = () => {
    const { instance } = useMsal()

    const handleLogin = () => {
        console.log('REACT_APP_CLIENT_ID', process.env.REACT_APP_CLIENT_ID)
        console.log('REACT_APP_AUTHORITY', process.env.REACT_APP_AUTHORITY)
        console.log('REACT_APP_REDIRECT_URI', process.env.REACT_APP_REDIRECT_URI)

        instance.loginPopup(loginRequest).catch(e => {
            console.error(e)
        })
    }

    return (
        <Button variant='secondary' className='ml-auto' onClick={() => handleLogin()}>
            Sign in
        </Button>
    )
}
