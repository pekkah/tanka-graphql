import * as React from "react";
import { TankaClient, TankaLink } from "@tanka/tanka-graphql-server-link";
import { RetryLink } from "apollo-link-retry";
import { ApolloLink } from 'apollo-link';
import GraphiQL from 'graphiql';
import { parse } from 'graphql';
import { execute } from 'apollo-link';

const client = new TankaClient('https://localhost:5000/graphql', {});
const tankaLink = new TankaLink(client);

const link = ApolloLink.from([
  new RetryLink(),
  tankaLink
]);

const fetcher = (operation) => {
  operation.query = parse(operation.query);
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