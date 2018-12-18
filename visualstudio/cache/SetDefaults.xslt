<xsl:stylesheet version="1.0"
		xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt"
		exclude-result-prefixes="msxsl"
		xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">

  <xsl:output method="xml" indent="yes" />
  <xsl:strip-space elements="*"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match='wix:File[@Id]'>
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:attribute name="DefaultVersion">$(var.FileVersion)</xsl:attribute>
      <xsl:attribute name="DefaultLanguage">1033</xsl:attribute>
      <xsl:attribute name="Vital">yes</xsl:attribute>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>