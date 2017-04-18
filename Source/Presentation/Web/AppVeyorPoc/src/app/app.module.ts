import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AppVeyorService } from './app-veyor.service';
import { ProjectsCustomComponent } from './projects-area/projects/projects.custom-component';
import { ProjectDetailsComponent } from './projects-area/project-details/project-details.component';
import { HistoryComponent } from './projects-area/history/history.component';
import { SettingsComponent } from './projects-area/settings/settings.component';
import { BuildComponent } from './projects-area/build-area/build/build.component';
import { ConsoleComponent } from './projects-area/build-area/console/console.component';
import { MessagesComponent } from './projects-area/build-area/messages/messages.component';
import { ArtifactsComponent } from './projects-area/build-area/artifacts/artifacts.component';

@NgModule({
  declarations: [
    AppComponent,
    ProjectsCustomComponent,
    ProjectDetailsComponent,
    HistoryComponent,
    SettingsComponent,
    BuildComponent,
    ConsoleComponent,
    MessagesComponent,
    ArtifactsComponent
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
