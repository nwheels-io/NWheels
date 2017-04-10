import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

@Component({
  selector: 'app-project-details',
  templateUrl: './project-details.component.html',
  styleUrls: ['../../assets/css/app/projects/project-details.component.css']
})

export class ProjectDetailsComponent implements OnInit {

  constructor(private route: ActivatedRoute) { }

  projectId: string;

  ngOnInit(): void {
    this.route.params.subscribe(params => this.projectId = params['projectId']);
  }
}