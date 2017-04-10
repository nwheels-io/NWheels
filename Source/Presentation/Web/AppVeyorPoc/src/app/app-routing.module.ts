import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ProjectsComponent } from './projects/projects.component';
import { ProjectDetailsComponent } from './projects/project-details.component';
import { HistoryComponent } from './projects/history.component';
import { SettingsComponent } from './projects/settings.component';
import { BuildComponent } from './projects/build/build.component';
import { ConsoleComponent } from './projects/build/console.component';
import { MessagesComponent } from './projects/build/messages.component';
import { ArtifactsComponent } from './projects/build/artifacts.component';

const routes: Routes = [
  { path: '', redirectTo: '/projects', pathMatch: 'full' },
  { path: 'projects', component: ProjectsComponent },
  {
    path: 'project/:projectId', component: ProjectDetailsComponent,
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
  }/*,
  { path: 'detail/:id', component: HeroDetailComponent },
  { path: 'heroes', component: HeroesComponent }*/
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})

export class AppRoutingModule { }