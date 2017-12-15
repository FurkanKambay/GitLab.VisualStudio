---
title: Release Notes
subtitle: What't new ?
comments: false
---



## What's new ?

**V1.0.0.122**

Fix HttpUtility.UrlEncode processing username or email causing problems that cannot be logged in

**V1.0.0.119**

Now update login mode is OAuth2, which can't be logon before because the new version of GitLab's API session has been discarded.

The two API login methods are supported in the login interface, and the old version of GitLab needs to be selected manually. The default is that the login mode is OAuth2 and V4 !


**V1.0.0.115**

1.You can select GitLab Api version .

**V1.0.0.112**

1.modify "Open On GitLab" to "GitLab"

**V1.0.0.95**

1. French, Japanese, German and other languages have been added, but these are Google's translations, so we need human translation!
2. Open on GitLab move to  submenu!
3. Fixed issue #3,Thanks luky92!
4. The selected code can create code snippets directly
5. When you create a project, you can select namespases.
6. GitLab's Api is updated from V3 to V4.


**V1.0.0.70**

1. GitLab login information associated with the solution, easy to switch GitLab server.
2. Enter the password and press enter to login GitLab server.
3. Now, We can login   with two  factor authentication.just enter the personal access token into the password field.

**V1.0.0.58** 

1. Support for Visual Studio 2017 
2. Fix bus.


**V1.0.0.40** 
 1. Right click on editor, if repository is hosted on GitLab Server , you can jump to master/current branch/current revision's blob page and blame/commits page. If selecting line(single, range) in editor, jump with line number fragment.
 2. Fix [#4](https://www.gitlab.com/maikebing/GitLab.VisualStudio/issues/4) [#5](https://www.gitlab.com/maikebing/GitLab.VisualStudio/issues/5) [#6](https://www.gitlab.com/maikebing/GitLab.VisualStudio/issues/6)
Official builds of this extension are available at [the official website](http://visualstudio.gitclub.cn).
