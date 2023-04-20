import { EnabledFeatures, EnabledFeaturesService } from 'domain/services/EnabledFeaturesService'
import { useService } from 'hooks/useService'
import React, { useEffect, useState } from 'react'

interface IEnabledFeaturesContext {
    enabledFeatures: EnabledFeatures
    isEnabledFeaturesLoading: boolean
}

export const EnabledFeaturesContext = React.createContext<IEnabledFeaturesContext>(
    {} as IEnabledFeaturesContext
)

export const EnabledFeaturesContextProvider = ({ children }: { children: React.ReactNode }) => {
    const context = useEnabledFeaturesContextProvider()
    return (
        <EnabledFeaturesContext.Provider value={context}>
            {children}
        </EnabledFeaturesContext.Provider>
    )
}

// Don't export because this should only be used via useContext(EnabledFeaturesContext)
const useEnabledFeaturesContextProvider = (): IEnabledFeaturesContext => {
    const enabledFeaturesService = useService(EnabledFeaturesService)
    const [isEnabledFeaturesLoading, setIsEnabledFeaturesLoading] = useState<boolean>(false)
    const [enabledFeatures, setEnabledFeatures] = useState<EnabledFeatures>({
        auth: false,
        sql: false,
        blobStorage: false,
    })

    useEffect(() => {
        setIsEnabledFeaturesLoading(true)
        enabledFeaturesService.getEnabledFeatures().then(enabledFeatures => {
            setEnabledFeatures(enabledFeatures)
            setIsEnabledFeaturesLoading(false)
        })
    }, [enabledFeaturesService])

    return {
        enabledFeatures,
        isEnabledFeaturesLoading,
    }
}
