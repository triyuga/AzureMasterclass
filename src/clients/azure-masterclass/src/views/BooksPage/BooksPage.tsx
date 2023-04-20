import { useIsAuthenticated } from '@azure/msal-react'
import { SignInButton } from 'components/SignInButton'
import { Book, BookService } from 'domain/services/BookService'
import { useService } from 'hooks/useService'
import { useEffect, useState } from 'react'
import { EditBookModal } from './EditBookModal'
import styles from './BooksPage.module.scss'
import { BooksTable } from './BooksTable'

export const BooksPage = () => {
    const bookService = useService(BookService)
    const isAuthenticated = useIsAuthenticated()

    const [books, setBooks] = useState<Book[]>([])
    const [isLoading, setIsLoading] = useState<boolean>(false)

    const [editingBook, setEditingBook] = useState<Book>()

    useEffect(() => {
        if (isAuthenticated) {
            setIsLoading(true)
            bookService
                .getBooks()
                .then(books => {
                    setIsLoading(false)
                    setBooks(books)
                })
                .catch(e => {
                    console.error(e)
                    setIsLoading(false)
                })
        }
    }, [bookService, isAuthenticated])

    const deleteBook = async (id: number) => {
        await bookService.deleteBook(id)
        setBooks(await bookService.getBooks())
    }

    const editBook = async (book: Book) => {
        setEditingBook(book)
    }

    const submitBook = async (book: Book) => {
        if (editingBook?.id === 0) {
            await bookService.addBook(book)
        } else {
            await bookService.updateBook(book)
        }
        setEditingBook(undefined)
        setBooks(await bookService.getBooks())
    }

    const cancelEditing = () => {
        setEditingBook(undefined)
    }

    const addNewBook = () => {
        setEditingBook({
            id: 0,
            name: '',
        })
    }

    return (
        <div className={styles.root}>
            <h1>Books</h1>
            {isAuthenticated && (
                <>
                    <BooksTable
                        books={books}
                        isLoading={isLoading}
                        editBook={editBook}
                        deleteBook={deleteBook}
                    ></BooksTable>
                    <button type='button' onClick={addNewBook}>
                        Add a Book
                    </button>
                    {editingBook && (
                        <EditBookModal
                            book={editingBook}
                            onSubmit={submitBook}
                            onClose={cancelEditing}
                        />
                    )}
                </>
            )}
            {!isAuthenticated && (
                <span>
                    Sign in to view books. <SignInButton />
                </span>
            )}
        </div>
    )
}
