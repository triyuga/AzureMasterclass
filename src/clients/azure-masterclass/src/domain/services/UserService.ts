import { HttpService } from './HttpService'

export interface User {
    email: string
    givenName: string
    surname: string
}

export class UserService extends HttpService {
    public getUser = () => {
        return this.get<User>('/api/User')
    }
}
