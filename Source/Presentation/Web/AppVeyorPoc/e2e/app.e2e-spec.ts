import { AppVeyorPocPage } from './app.po';

describe('app-veyor-poc App', () => {
  let page: AppVeyorPocPage;

  beforeEach(() => {
    page = new AppVeyorPocPage();
  });

  it('should display message saying "List of projects(custom)"', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('List of projects(custom)');
  });
});
