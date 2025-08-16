import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Post } from '../../../models/post.model';


@Component({
  selector: 'app-post-item',
  imports: [],
  templateUrl: './post-item.html',
  styleUrl: './post-item.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostItem {
  @Input() post!: Post;
}