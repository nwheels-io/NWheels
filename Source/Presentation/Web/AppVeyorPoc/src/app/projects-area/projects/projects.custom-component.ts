import { Component, OnInit } from '@angular/core';

import { Project } from '../project';
import { ProjectsComponent } from './projects.component';
import { AppVeyorService } from '../../app-veyor.service';

@Component({
  templateUrl: './projects.custom-component.html',
  styleUrls: ['./projects.custom-component.css']
})

export class ProjectsCustomComponent extends ProjectsComponent {

  constructor(protected appVeyorService: AppVeyorService) { super(appVeyorService); }
  
}