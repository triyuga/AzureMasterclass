import ReactDOM from 'react-dom/client'
import { App } from './App'
import reportWebVitals from './reportWebVitals'
import {
    AuthenticationResult,
    EventMessage,
    EventType,
    PublicClientApplication,
} from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import { msalConfig } from './domain/authConfig'
import { EnabledFeaturesContextProvider } from 'contexts/EnabledFeaturesContext'

import './index.scss'
import 'bootstrap/dist/css/bootstrap.min.css'

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement)

export const msalInstance = new PublicClientApplication(msalConfig)
const accounts = msalInstance.getAllAccounts()
if (accounts.length > 0) {
    msalInstance.setActiveAccount(accounts[0])
}

msalInstance.addEventCallback((event: EventMessage) => {
    if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
        const payload = event.payload as AuthenticationResult
        const account = payload.account
        msalInstance.setActiveAccount(account)
    }
})

root.render(
    // Note: React.StrictMode causes double render, disable for now! See https://github.com/facebook/create-react-app/issues/12363
    // <React.StrictMode>
    <MsalProvider instance={msalInstance}>
        <EnabledFeaturesContextProvider>
            <App />
        </EnabledFeaturesContextProvider>
    </MsalProvider>
    // </React.StrictMode>
)

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals()
