import {
  Component,
  signal,
  computed,
  effect,
  ElementRef,
  inject,
  OnInit,
  OnDestroy,
  output,
  Injector,
} from '@angular/core';
import { CommonModule } from '@angular/common';

interface SearchItem {
  title: string;
  subtitle: string;
}

@Component({
  selector: 'app-combobox',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './combobox.html',
  styleUrl: './combobox.css',
})
export class Combobox {
  private host = inject(ElementRef);
  private injector = inject(Injector);
  private allItems: SearchItem[] = [];
  query = signal('');
  isOpen = signal(false);

  output = output<string>();

  results = computed(() =>
    this.allItems.filter((item) => item.title.toLowerCase().includes(this.query().toLowerCase()))
  );

  constructor() {
    for (let i = 1; i <= 300; i++) {
      this.allItems.push({
        title: `Item ${i}`,
        subtitle: `Subtitle for item ${i}`,
      });
    }
  }

  onInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.query.set(value);
    this.isOpen.set(!!value); // open if query is non-empty
  }

  // --- Click outside handling ---
  private onDocumentClick = (event: MouseEvent) => {
    if (!this.host.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  };

  ngOnInit() {
    document.addEventListener('click', this.onDocumentClick, true);

    // emit
    effect(
      () => {
        this.output.emit(this.query());
      },
      { allowSignalWrites: true, injector: this.injector }
    );
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.onDocumentClick, true);
  }
}
