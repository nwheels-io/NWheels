import { Component, OnInit } from '@angular/core';

import { Project } from './project';
import { AppVeyorService } from '../app-veyor.service';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['../../assets/css/app/projects/projects.component.css']
})

export class ProjectsComponent implements OnInit {

  projects: Project[];

  constructor(private appVeyorService: AppVeyorService) { }

  ngOnInit(): void {
    this.appVeyorService
      .getProjects()
      .then(projects => this.projects = projects);
  }
}