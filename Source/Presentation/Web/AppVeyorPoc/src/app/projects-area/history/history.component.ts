import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Project } from '../project';
import { AppVeyorService } from '../../app-veyor.service';

@Component({
  templateUrl: './history.custom-component.html',
  styleUrls: ['./history.component.css']
})

export class HistoryComponent implements OnInit {

  private sub: any;
  project: Project;

  constructor( private route: ActivatedRoute, private appVeyorService: AppVeyorService) { }

  ngOnInit(): void {
    this.sub = this.route.parent.params.subscribe(params => 
      this.appVeyorService.getBuildsByProjectName(params['projectName']).then(project => this.project = project));
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}