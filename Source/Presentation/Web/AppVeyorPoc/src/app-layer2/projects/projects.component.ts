import { Component, OnInit } from '@angular/core';

import { Layer1ProjectsComponent } from '../../app-layer1/projects/projects.component';
import { AppVeyorService } from '../../app/app-veyor.service';

@Component({
  templateUrl: '../../app-layer1/projects/projects.component.html',
  styleUrls: ['./projects.component.css']
})

export class Layer2ProjectsComponent extends Layer1ProjectsComponent {

  constructor(protected appVeyorService: AppVeyorService) { super(appVeyorService); }
  
}