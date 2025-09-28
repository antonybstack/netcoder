import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Combobox } from './components/combobox/combobox';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Combobox],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('app');
}
