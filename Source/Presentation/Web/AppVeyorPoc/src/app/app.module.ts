import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AppVeyorService } from './app-veyor.service';
import { ProjectsComponent } from './projects/projects.component';
import { ProjectDetailsComponent } from './projects/project-details.component';
import { HistoryComponent } from './projects/history.component';
import { SettingsComponent } from './projects/settings.component';
import { BuildComponent } from './projects/build/build.component';
import { ConsoleComponent } from './projects/build/console.component';
import { MessagesComponent } from './projects/build/messages.component';
import { ArtifactsComponent } from './projects/build/artifacts.component';

import { Layer3ProjectsComponent } from '../app-layer3/projects/projects.component'

@NgModule({
  declarations: [
    AppComponent,
    ProjectsComponent,
    ProjectDetailsComponent,
    HistoryComponent,
    SettingsComponent,
    BuildComponent,
    ConsoleComponent,
    MessagesComponent,
    ArtifactsComponent,

    Layer3ProjectsComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    AppRoutingModule
  ],
  providers: [AppVeyorService],
  bootstrap: [AppComponent]
})
export class AppModule { }
