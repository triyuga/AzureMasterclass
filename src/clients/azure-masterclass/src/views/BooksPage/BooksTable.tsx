import { Book } from 'domain/services/BookService'
import { Table } from 'react-bootstrap'

interface BooksTableProps {
    books: Book[]
    isLoading: boolean
    editBook: (book: Book) => void
    deleteBook: (id: number) => void
}

export const BooksTable = ({ books, isLoading, editBook, deleteBook }: BooksTableProps) => (
    <Table striped bordered variant=''>
        <thead>
            <tr>
                <th>Id</th>
                <th>Name</th>
                <th>Description</th>
                <th>Cover</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            {books.length > 0 ? (
                books.map(b => (
                    <tr key={b.id}>
                        <td>{b.id}</td>
                        <td>{b.name}</td>
                        <td>{b.description}</td>
                        <td>{b.cover && <img src={`data:png;base64,${b.cover}`} alt='cover' />}</td>
                        <td>
                            <button type='button' onClick={() => deleteBook(b.id)}>
                                Delete
                            </button>
                            <button type='button' onClick={() => editBook(b)}>
                                Edit
                            </button>
                        </td>
                    </tr>
                ))
            ) : (
                <tr>
                    <td colSpan={5} align='center'>
                        {isLoading ? 'Loading...' : 'There are no books yet'}
                    </td>
                </tr>
            )}
        </tbody>
    </Table>
)
