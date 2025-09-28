import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Combobox } from './components/combobox/combobox';
import { Sample } from './components/sample/sample';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Combobox, Sample],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('app');
}
