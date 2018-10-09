import * as React from 'react';
import { Provider } from 'react-redux'
import { store, Playground } from 'graphql-playground-react'
import { Session } from 'graphql-playground-react/lib/state/sessions/reducers';
import { Client, SignalrLink } from 'fugu-graphql-server-apollo-link';

var client = new Client("http://localhost:56794/graphql-ws");
var link = new SignalrLink(client);

class App extends React.Component {
  createLink = (session: Session) => {
    console.log("Link", link)
    return {
      link: link
    };
  };

  public render() {
    return (
      <Provider store={store}>
        <Playground endpoint="http://localhost:56794/graphql"
          subscriptionEndpoint="http://localhost:56794/graphql-ws"
          createApolloLink={this.createLink}
        />
      </Provider>
    );
  }
}

export default App;
