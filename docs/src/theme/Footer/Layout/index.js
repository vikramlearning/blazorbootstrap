/**
 * Swizzled (ejected) from @docusaurus/theme-classic Footer/Layout.
 * Renders the Blazor Bootstrap footer: brand block, link columns and a bottom bar,
 * matching the demo site's footer.
 */
import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import {ThemeClassNames} from '@docusaurus/theme-common';
import GitHubIcon from '@theme/Icon/Socials/GitHub';
import XIcon from '@theme/Icon/Socials/X';
import LinkedInIcon from '@theme/Icon/Socials/LinkedIn';
import styles from './styles.module.css';

const socials = [
  {label: 'Blazor Bootstrap on GitHub', href: 'https://github.com/vikramlearning/blazorbootstrap', Icon: GitHubIcon},
  {label: 'Blazor Bootstrap on X', href: 'https://twitter.com/blazorbootstrap', Icon: XIcon},
  {label: 'Blazor Bootstrap on LinkedIn', href: 'https://www.linkedin.com/groups/14161025', Icon: LinkedInIcon},
];

export default function FooterLayout({style, links, logo, copyright}) {
  return (
    <footer
      className={clsx(ThemeClassNames.layout.footer.container, 'footer', styles.footer, {
        'footer--dark': style === 'dark',
      })}>
      <div className="container container-fluid">
        <div className="row">
          <div className={clsx('col col--4', styles.brand)}>
            {logo}
            <p className={styles.tagline}>
              Free, open-source Blazor components built on Bootstrap 5, made by the Blazor Bootstrap team and our contributors.
            </p>
            <ul className={styles.socials}>
              {socials.map(({label, href, Icon}) => (
                <li key={href}>
                  <a href={href} target="_blank" rel="noopener noreferrer" aria-label={label} className={styles.social}>
                    <Icon aria-hidden="true" focusable="false" />
                  </a>
                </li>
              ))}
            </ul>
          </div>
          <div className="col col--8">{links}</div>
        </div>
        <div className={styles.bottom}>
          {copyright}
          <span className={styles.separator} aria-hidden="true">·</span>
          <span>
            Code licensed{' '}
            <Link href="https://github.com/vikramlearning/blazorbootstrap/blob/main/LICENSE.txt" rel="license noopener noreferrer">
              Apache License 2.0
            </Link>
          </span>
        </div>
      </div>
    </footer>
  );
}
