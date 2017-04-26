import { Component, OnInit } from '@angular/core';

import { Project } from '../project';
import { AppVeyorService } from '../../app-veyor.service';

@Component({
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})

export class ProjectsComponent implements OnInit {

  projects: Project[];// = new Array<Project>();

  constructor(protected appVeyorService: AppVeyorService) { }

  ngOnInit(): void {
    this.appVeyorService
      .getProjects()
      .subscribe(projects => this.projects = projects);
  }
}