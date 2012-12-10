--- 
title: Jumping over to GitHub
tags: 
- GitHub
- Prexonite
- Assembla
- Git
- Mercurial
- Scala
type: post
layout: post
---

Ever since I moved Prexonite to its [mercurial repository at assembla][hg-assembla] I've been a bit jealous at open source projects over on [GitHub][github].
GitHub is just a nicer web site and while assembla has a very powerful issue tracking system, but as a solo developer it was maybe a bit too much.
[Bitbucket][bitbucket], on the other hand, felt too basic; always five steps behind GitHub.

The main reason, I didn't migrate Prexonite over to GitHub was actually [git][git] itself. 
At first, the main argument against git was the abysmal quality of Windows ports available at the time. 
No I don't want to use Cygwin or MSYS or any other non-Windows shell. 
I'm using PowerShell, I'm used to it and it works perfectly on Windows.

But even if we assume that there would have been a nice git port for Windows, git still makes things that are easy in Mercurial much more difficult.
Proponents of git will of course tout this as an advantage and indeed you can do so much <em>more</em> with git.
However, if you don't care about rebase and just want a distributed version control system that works, Mercurial is perfectly fine.

In the last year we have seen the two tools, git and Mercurial, align more and more: Mercurial now comes with built-in support for rebase and tracking bookmarks, which are essentially git-style branches whereas git has become more and more user friendly.
The arrival of [GitHub for Windows][windows.github.com], finally, has removed the last blockade on the way over to GitHub for me.

While I currently don't know of any graphical Git client that comes even close to [TortoiseHg][thg], there is a ton of documentation and tutorials on git available on the 'net. 
Learning new things has always been fun.

The decision was made, on to GitHub we move.

### Repository conversion 

At the time I decided to move to GitHub I had already started using tracking-bookmarks for feature branches instead of Mercurial's rather heavyweight named branches. 
Accordingly I needed a tool that not just recognised named branches but also the rather new Mercurial bookmarks.
It came down to using either [hg-git][hg-git], in which you take a Mercurial repository and push to a git remote or [git-hg][git-hg], which does the exact opposite, taking a git repository and pulling from a Mercurial one. 
(Both tools can work in both directions, but I was only interested in the move from Mercurial to git)

I ended up using ***git-hg*** because it seemed produced a cleaner git repository.
Adding a README.md was all that was left.

### Moving issues to GitHub

During the time at Assembla, I had created a small collection of issues with a bit over a hundred entries. 
As GitHub, too, has an issue tracking system I felt that it would have been a shame to just let these issues rot over at assembla.
So I decided to make use of Assembla's XML-export feature and GitHub's API.

While Assembla's ticket system is quite extensive with custome fields and enumerations etc. GitHub's simple flat tag-based approach works just as well for a project of the size of Prexonite.
It did, however, mean that I would have to do a fair bit of translation between the two systems.

The first step was to parse the exported tickets from the generated XML file to get an overview of what would be required. 
Written in Scala, this "**survey**" took about 70 lines of code and listed all keywords, milestones and project components I had used over at Assembla.

Since some form of mapping would be necessary anyway (e.g., to get rid of different hyphenations of `partial-application`), I decided to combine redundant keywords into a handful of more meaningful tags on the GitHub side.
In order to make the issue import simpler, I created the GitHub tags ahead of time by hand.

With the keyword, component, issue state and milestone mappings in hand, I went ahead and used the [dispatch-github][dispatch-github] library for Scala to add the translated issues to the GitHub project for Prexonite. 
While the actual import had its problems (had to introduce `Thread.Sleep` here and there), in the end I managed to get all my tickets from assembla over to GitHub.
The Scala program to do so clocked in at 300-400 lines of code, depending on whether you count the survey and mapping configuration or not.
While dispatch-github was not in the best of states at the time, I would definitely keep using Scala. 
For such a small project it's a very efficient language (in terms of programmer productivity) and in a situation where I'm making API calls to a live system, I'd rather have a good type system backing me up.

[hg-assembla]: https://www.assembla.com/code/prx/mercurial/nodes
[github]: https://github.com
[bitbucket]: https://bitbucket.org/
[git]: http://git-scm.com/
[hg]: http://mercurial.selenic.com/
[windows.github.com]: http://windows.github.com/
[thg]: http://tortoisehg.bitbucket.org/
[git-hg]: https://github.com/cosmin/git-hg
[hg-git]: https://github.com/schacon/hg-git
[dispatch-github]: https://github.com/andreazevedo/dispatch-github