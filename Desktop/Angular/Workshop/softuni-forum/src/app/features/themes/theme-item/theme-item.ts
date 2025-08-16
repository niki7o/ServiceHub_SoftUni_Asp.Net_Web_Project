import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Theme } from '../../../models/theme.model';


@Component({
  selector: 'app-theme-item',
  imports: [],
  templateUrl: './theme-item.html',
  styleUrl: './theme-item.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ThemeItem {
  @Input() theme!: Theme;
}