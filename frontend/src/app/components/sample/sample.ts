import { JsonPipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { Component, input, signal } from '@angular/core';
import { Combobox } from '../combobox/combobox';

@Component({
  selector: 'app-sample',
  imports: [JsonPipe, Combobox],
  templateUrl: './sample.html',
  styleUrl: './sample.css',
})
export class Sample {
  onComboboxOutput($event: string) {
    this.searchInput.set($event);
  }
  searchInput = signal<string>('');

  // example.com
  searchResult = httpResource(() => {
    console.log('Fetching searchResult for', this.searchInput());
    return `https://jsonplaceholder.typicode.com/posts?userId=1`;
  });
}
