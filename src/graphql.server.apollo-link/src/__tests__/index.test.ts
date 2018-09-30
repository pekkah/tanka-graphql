import { Client, SignalrLink} from '../index';

describe('exports', () => {
  test('Client', () => {
    expect(Client).toBeDefined();
  });

  test('SignalrLink', () => {
    expect(SignalrLink).toBeDefined();
  });
});