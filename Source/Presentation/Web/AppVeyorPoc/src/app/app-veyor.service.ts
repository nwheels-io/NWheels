import { Injectable } from '@angular/core';
import { Headers, Http, Request, Response, RequestMethod } from '@angular/http';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import "rxjs/add/operator/catch";
import "rxjs/add/operator/publishReplay";
import "rxjs/add/operator/filter";
import "rxjs/add/operator/first";
import 'rxjs/add/operator/mergeMap';
import "rxjs/add/observable/throw";

import { Project } from './projects-area/project';
import { Configuration } from '../../configuration';

@Injectable()
export class AppVeyorService {

  private headers = new Headers({
    'Authorization': `Bearer ${Configuration.appVeyorToken}`,
    'Content-type': 'application/xml'
  });
  private appVeyorUrl = 'https://ci.appveyor.com/api';  // URL to web api
  private projects: Observable<Project[]>;

  constructor(private http: Http) { }

  getProjects(): Observable<Project[]> {
    if (!this.projects) {
      this.projects = this.sendRequest<Project[]>(RequestMethod.Get, `${this.appVeyorUrl}/projects`)
        .publishReplay(1)
        .refCount();
    }
    return this.projects;
  }

  clearCache() {
    this.projects = null;
  }
  
  getProjectByName(projectName: string): Observable<Project> {
    return this.getProjects()
      .mergeMap(projects => {
        return projects.filter(project => project.name === projectName);
      }).first();
  }
  
  getBuildsByProjectName(projectName: string): Observable<Project> {
    return this.getProjectByName(projectName).flatMap(project =>
      this.sendRequest<Project>(
        RequestMethod.Get,
        `${this.appVeyorUrl}/projects/${project.accountName}/${project.slug}/history?recordsNumber=10`)
    );
  }
  
  private sendRequest<T>(verb: RequestMethod, url: string, body?: T): Observable<T> {
    return this.http.request(
      new Request({
        headers: this.headers,
        method: verb,
        url: url,
        body: body
      }))
      .map(response => response.json())
      .catch((error: Response) => Observable.throw(`Network Error: ${error.statusText} (${error.status})`));;
  } 
}