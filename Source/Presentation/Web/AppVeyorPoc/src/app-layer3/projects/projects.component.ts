import { Component, OnInit } from '@angular/core';

import { Layer2ProjectsComponent } from '../../app-layer2/projects/projects.component';
import { AppVeyorService } from '../../app/app-veyor.service';

@Component({
  templateUrl: '../../app-layer1/projects/projects.component.html',
  styleUrls: ['../../app-layer2/projects/projects.component.css']
})

export class Layer3ProjectsComponent extends Layer2ProjectsComponent {

  constructor(protected appVeyorService: AppVeyorService) { super(appVeyorService); }

  //some additional behavior
}