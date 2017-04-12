import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-build',
  templateUrl: './build.component.html'
})

export class BuildComponent implements OnInit {

  private sub: any;
  buildId: string;

  constructor(private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.sub = this.route.params.subscribe(params => this.buildId = params['buildId']);
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}