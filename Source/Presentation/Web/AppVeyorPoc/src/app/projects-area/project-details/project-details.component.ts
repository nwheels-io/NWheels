import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Project } from '../project';
import { AppVeyorService } from '../../app-veyor.service';

@Component({
  templateUrl: './project-details.component.html',
  styleUrls: ['./project-details.custom-component.css']
})

export class ProjectDetailsComponent implements OnInit {

  private sub: any;
  project: Project;

  constructor(private route: ActivatedRoute, private appVeyorService: AppVeyorService) { }

  ngOnInit(): void {
    this.sub = this.route.params.subscribe(params =>
      this.appVeyorService.getProjectByName(params['projectName']).subscribe(project => this.project = project));
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}