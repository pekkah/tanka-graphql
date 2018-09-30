import { ApolloLink, Operation, FetchResult, Observable } from 'apollo-link';

export class InitializationResult {
    
}

export class Client {
    constructor(private url: string) {
    }

    public async Initialize() : Promise<InitializationResult> {
        
        return Promise.resolve(new InitializationResult());
    }
}

export class SignalrLink extends ApolloLink {
    constructor(private client: Client) {
        super()
    }
}