import { Operation } from "apollo-link";

export class Request {
  constructor(public id: string, public operation: Operation) {}
}
