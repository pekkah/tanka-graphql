import { greet } from '../index';

describe('This is a simple test', () => {
  test('heck the greet function', () => {
    expect(greet()).toEqual('Hello, World!');
  });
});