/**
 * Swizzled (wrapped) @docusaurus/theme-classic Footer: shows the team's other libraries above the footer on every page.
 */
import React from 'react';
import Footer from '@theme-original/Footer';
import MoreLibraries from '@site/src/components/MoreLibraries';

export default function FooterWrapper(props) {
  return (
    <>
      <MoreLibraries />
      <Footer {...props} />
    </>
  );
}
