import { EnabledFeaturesContext } from 'contexts/EnabledFeaturesContext'
import { useContext } from 'react'
import { Table } from 'react-bootstrap'
import styles from './HomePage.module.scss'

export const HomePage = () => {
    const { enabledFeatures } = useContext(EnabledFeaturesContext)
    return (
        <div className={styles.root}>
            <h1>Home</h1>
            <p>
                This is the home page. It calls a public endpoint to get a list of features enabled
                in the API project.
            </p>

            <Table striped bordered variant=''>
                <thead>
                    <tr>
                        <th>Feature</th>
                        <th>Is enabled?</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>Auth</td>
                        <td>{enabledFeatures.auth ? 'yes' : 'no'}</td>
                        <td>Auth is blah blah blah</td>
                    </tr>
                    <tr>
                        <td>SQL</td>
                        <td>{enabledFeatures.sql ? 'yes' : 'no'}</td>
                        <td>SQL is blah blah blah</td>
                    </tr>
                    <tr>
                        <td>BlobStorage</td>
                        <td>{enabledFeatures.blobStorage ? 'yes' : 'no'}</td>
                        <td>BlobStorage is blah blah blah</td>
                    </tr>
                </tbody>
            </Table>
        </div>
    )
}
