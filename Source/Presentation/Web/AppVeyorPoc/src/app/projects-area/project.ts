import { Build } from './build';

export class Project {
  projectId: number;
  accountId: number;
  accountName: string;
  name: string;
  slug: string;
  repositoryName: string;
  repositoryBranch: string;
  builds: Build[];
}