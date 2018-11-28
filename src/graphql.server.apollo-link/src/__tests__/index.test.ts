import { FuguClient, FuguLink } from "../.";

describe("exports", () => {
  test("Client", () => {
    expect(FuguClient).toBeDefined();
  });

  test("SignalrLink", () => {
    expect(FuguLink).toBeDefined();
  });
});
