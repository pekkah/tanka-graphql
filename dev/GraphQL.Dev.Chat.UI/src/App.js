import * as React from "react";
import { TankaClient, TankaLink } from "@tanka/tanka-graphql-server-link";
import {HttpTransportType} from "@microsoft/signalr";
import { ApolloLink } from '@apollo/client';
import GraphiQL from 'graphiql';
import { parse } from 'graphql';
import { execute } from '@apollo/client';

const client = new TankaClient('https://localhost:5000/graphql', {
  connection: {
    transport: HttpTransportType.WebSockets
  }
});
const tankaLink = new TankaLink(client);

const link = ApolloLink.from([
  tankaLink
]);

const fetcher = (operation) => {
  operation.query = parse(operation.query);
  console.log('op', operation);
  return execute(link, operation);
};

class Home extends React.Component {
  render() {
    return (
      <GraphiQL fetcher={fetcher}/>
    );
  }
}

export default Home;