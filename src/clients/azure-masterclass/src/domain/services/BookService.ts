import { HttpService } from './HttpService'

export interface Book {
    id: number
    name?: string
    description?: string
    cover?: string
}

export class BookService extends HttpService {
    public getBooks = () => {
        return this.get<Book[]>('/api/Books')
    }

    public deleteBook = (id: number) => {
        return this.delete(`/api/Books/${id}`)
    }

    public updateBook = (book: Book) => {
        return this.put(`/api/Books`, book)
    }

    public addBook = (book: Book) => {
        return this.post(`/api/Books`, book)
    }
}
