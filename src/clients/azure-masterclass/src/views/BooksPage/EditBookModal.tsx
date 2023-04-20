import { Book } from 'domain/services/BookService'
import { FC, useEffect, useState } from 'react'
import Modal from 'react-bootstrap/Modal'

interface IEditBookModalProps {
    book: Book
    onClose: () => void
    onSubmit: (book: Book) => void | Promise<void>
}

export const EditBookModal: FC<IEditBookModalProps> = ({ book, onSubmit, onClose }) => {
    const isNew = book.id === 0

    const handleSubmit = () => {
        if (!!editingBook) {
            onSubmit(editingBook)
        }
    }

    const [editingBook, setEditingBook] = useState<Book | undefined>(undefined)

    useEffect(() => {
        setEditingBook(book)
    }, [book])

    const loadImage: React.ChangeEventHandler<HTMLInputElement> = e => {
        if (e.target.files?.length) {
            const file = e.target.files[0]
            const reader = new FileReader()

            reader.onload = e => {
                const bStr = e.target?.result as string
                setEditingBook({ ...editingBook, id: editingBook?.id ?? 0, cover: btoa(bStr) })
            }
            reader.readAsBinaryString(file)
        }
    }

    return editingBook ? (
        <Modal show onHide={onClose}>
            <form>
                <Modal.Header closeButton>
                    <Modal.Title>{isNew ? 'New Book' : `Edit Book ${book.id}`}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div>
                        <label>Name</label>
                        <input
                            value={editingBook?.name || ''}
                            onChange={e => setEditingBook({ ...editingBook, name: e.target.value })}
                        ></input>
                    </div>

                    <div>
                        <label>Description</label>
                        <input
                            value={editingBook?.description || ''}
                            onChange={e =>
                                setEditingBook({ ...editingBook, description: e.target.value })
                            }
                        ></input>
                    </div>
                    <div>
                        <label>Cover</label>
                        <input type='file' accept='image/png' onChange={loadImage} />
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <button type='button' onClick={onClose}>
                        Close
                    </button>
                    <button type='button' onClick={handleSubmit}>
                        {isNew ? 'Add' : 'Update'}
                    </button>
                </Modal.Footer>
            </form>
        </Modal>
    ) : null
}
