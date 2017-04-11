import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';

import { Project } from './projects/project';
import { AppVeyorServiceToken } from './app-veyor-service-token';

@Injectable()
export class AppVeyorService {

  private headers = new Headers({
    'Authorization': `Bearer ${AppVeyorServiceToken.token}`,
    'Content-type': 'application/xml'
  });
  private appVeyorUrl = 'https://ci.appveyor.com/api';  // URL to web api
  private projects: Project[];

  constructor(private http: Http) { }

  getProjects(): Promise<Project[]> {
    if (this.projects) {
      return new Promise((resolve, reject) => resolve(this.projects));
    }
    else {
      return this.http.get(`${this.appVeyorUrl}/projects`, { headers: this.headers })
        .toPromise()
        .then(response => {
          this.projects = response.json();
          return this.projects;
        })
        .catch(this.handleError);
    }
  }

  getProjectByName(projectName: string): Promise<Project> {
    return this.getProjects().then(projects => projects.find(project => project.name == projectName));
  }

  getBuildsByProjectName(projectName: string): Promise<Project> {
    //GET /api/projects/{accountName}/{projectSlug}/history?recordsNumber={records-per-page}[&startBuildId={buildId}&branch={branch}]
    return this.getProjectByName(projectName).then(project =>

      this.http.get(
        `${this.appVeyorUrl}/projects/${project.accountName}/${project.slug}/history?recordsNumber=10`,
        { headers: this.headers })
        .toPromise()
        .then(response => {
          return response.json() as Project;
        })
        .catch(this.handleError)
    );
  }

  private handleError(error: any): Promise<any> {
    console.error('An error occurred', error); // for demo purposes only
    return Promise.reject(error.message || error);
  }
}