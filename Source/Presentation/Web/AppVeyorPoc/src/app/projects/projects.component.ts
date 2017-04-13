import { Component, OnInit } from '@angular/core';

import { Project } from './project';
import { AppVeyorService } from '../app-veyor.service';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})

export class ProjectsComponent implements OnInit {

  projects: Project[];

  constructor(protected appVeyorService: AppVeyorService) { }

  ngOnInit(): void {
    this.appVeyorService
      .getProjects()
      .then(projects => this.projects = projects);
  }
}