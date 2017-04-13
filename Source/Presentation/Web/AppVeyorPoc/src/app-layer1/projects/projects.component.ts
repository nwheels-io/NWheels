import { Component, OnInit } from '@angular/core';

import { ProjectsComponent } from '../../app/projects/projects.component';
import { AppVeyorService } from '../../app/app-veyor.service';

@Component({
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})

export class Layer1ProjectsComponent extends ProjectsComponent {

  constructor(protected appVeyorService: AppVeyorService) { super(appVeyorService); }
  
}