import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ProjectsCustomComponent } from './projects-area/projects/projects.custom-component';
import { ProjectDetailsComponent } from './projects-area/project-details/project-details.component';
import { HistoryComponent } from './projects-area/history/history.component';
import { SettingsComponent } from './projects-area/settings/settings.component';
import { BuildComponent } from './projects-area/build-area/build/build.component';
import { ConsoleComponent } from './projects-area/build-area/console/console.component';
import { MessagesComponent } from './projects-area/build-area/messages/messages.component';
import { ArtifactsComponent } from './projects-area/build-area/artifacts/artifacts.component';

const routes: Routes = [
  { path: '', redirectTo: '/projects', pathMatch: 'full' },
  { path: 'projects', component: ProjectsCustomComponent },
  {
    path: 'project/:projectName', component: ProjectDetailsComponent,
    children: [
      { path: 'history', component: HistoryComponent },
      { path: 'settings', component: SettingsComponent },
      { path: 'build/:buildId', component: BuildComponent,
        children: [
          { path: '', redirectTo: 'console', pathMatch: 'full' },
          { path: 'console', component: ConsoleComponent },
          { path: 'messages', component: MessagesComponent },
          { path: 'artifacts', component: ArtifactsComponent }
        ]
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})

export class AppRoutingModule { }