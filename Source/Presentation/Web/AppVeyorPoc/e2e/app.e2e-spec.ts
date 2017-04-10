import { AppVeyorPocPage } from './app.po';

describe('app-veyor-poc App', () => {
  let page: AppVeyorPocPage;

  beforeEach(() => {
    page = new AppVeyorPocPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
